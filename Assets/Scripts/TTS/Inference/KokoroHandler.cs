using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Unity.InferenceEngine.Samples.TTS.Assets;
using Unity.InferenceEngine.Samples.TTS.Utils;
using UnityEngine;

namespace Unity.InferenceEngine.Samples.TTS.Inference
{
    public class KokoroHandler: IDisposable
    {
        const string k_KokoroModelPath = "onnx/model";
        const string k_VoicesFolderPath = "voices";

        Model m_Model;
        Worker m_Worker;
        readonly BackendType m_BackendType;
        readonly SemaphoreSlim m_ExecutionLock = new(1, 1);
        bool m_Disposed;

        public KokoroHandler(BackendType backendType = BackendType.CPU, bool lazyLoadModel = true)
        {
            m_BackendType = backendType;

            if (!lazyLoadModel)
                LoadModelIfMissing();
        }

        void LoadModelIfMissing()
        {
            ThrowIfDisposed();

            if (m_Model != null)
                return;

            var modelAsset = Resources.Load<ModelAsset>(k_KokoroModelPath);
            if (modelAsset == null)
                throw new FileNotFoundException(
                    $"Kokoro model was not found at Resources/{k_KokoroModelPath}.onnx.");

            m_Model = ModelLoader.Load(modelAsset);

            m_Worker = new Worker(m_Model, m_BackendType);
        }

        public async Task<Tensor<float>> Execute(int[] inputIds, float speed, Voice voice)
        {
            if (inputIds == null || inputIds.Length == 0)
                throw new ArgumentException("At least one input token is required.", nameof(inputIds));
            if (speed <= 0f)
                throw new ArgumentOutOfRangeException(nameof(speed), "Speech speed must be greater than zero.");
            if (voice?.Tensor == null)
                throw new ArgumentException("A loaded voice is required.", nameof(voice));

            // Add the pad ids
            var paddedInputIds = new int[inputIds.Length + 2];
            paddedInputIds[0] = 0;
            Array.Copy(inputIds, 0, paddedInputIds, 1, inputIds.Length);
            paddedInputIds[^1] = 0;

            using var inputIdsTensor = new Tensor<int>(new TensorShape(1, paddedInputIds.Length), paddedInputIds);
            using var speedTensor = new Tensor<float>(new TensorShape(1), new[] { speed });
            using var voiceTensor = await GetVoiceVector(inputIdsTensor, voice.Tensor);

            return await Execute(inputIdsTensor, voiceTensor, speedTensor);
        }

        public async Task<Tensor<float>> Execute(Tensor<int> inputIdsTensor, Tensor<float> voiceTensor, Tensor<float> speedTensor)
        {
            if (inputIdsTensor == null)
                throw new ArgumentNullException(nameof(inputIdsTensor));
            if (voiceTensor == null)
                throw new ArgumentNullException(nameof(voiceTensor));
            if (speedTensor == null)
                throw new ArgumentNullException(nameof(speedTensor));

            await m_ExecutionLock.WaitAsync();
            try
            {
                LoadModelIfMissing();

                m_Worker.Schedule(inputIdsTensor, voiceTensor, speedTensor);
                using var result = m_Worker.PeekOutput() as Tensor<float>
                    ?? throw new InvalidOperationException("Kokoro returned an unexpected output tensor type.");
                using var output = await result.ReadbackAndCloneAsync();

                return KokoroOutputProcessor.Apply2NotchFiltering(output);
            }
            finally
            {
                m_ExecutionLock.Release();
            }
        }

        public static List<Voice> GetVoices()
        {
            var voices = new List<Voice>();
            var voicesList = VoicesUtils.GetVoicesList();

            foreach (var file in voicesList)
            {
                var resourcePath = $"{k_VoicesFolderPath}/{file}";
                var voiceAsset = Resources.Load<RawBytesAsset>(resourcePath);

                if (voiceAsset == null)
                    throw new FileNotFoundException(
                        $"Voice '{file}' was not found at Resources/{resourcePath}.bin.");

                var voiceData = voiceAsset.bytes;
                if (voiceData == null || voiceData.Length == 0 || voiceData.Length % (256 * sizeof(float)) != 0)
                    throw new InvalidDataException($"Voice '{file}' contains invalid style-vector data.");

                var voiceArray = new float[voiceData.Length / sizeof(float)];
                Buffer.BlockCopy(voiceData, 0, voiceArray, 0, voiceData.Length);

                var styleShape = new TensorShape(voiceArray.Length / 256, 1, 256);
                var tensor = new Tensor<float>(styleShape, voiceArray);
                var voice = new Voice(file, tensor);
                voices.Add(voice);
            }

            return voices;
        }

        async Task<Tensor<float>> GetVoiceVector(Tensor<int> inputIds, Tensor<float> voice)
        {
            var graph = new FunctionalGraph();
            var tokenInput = graph.AddInput<float>(voice.shape, "voice");
            var output = tokenInput[inputIds.count];
            graph.AddOutput(output, "output");
            var model = graph.Compile();

            using var worker = new Worker(model, m_BackendType);
            worker.Schedule(voice);
            using var result = worker.PeekOutput() as Tensor<float>;
            return await result.ReadbackAndCloneAsync();
        }

        public void Dispose()
        {
            if (m_Disposed)
                return;

            m_Disposed = true;
            m_Worker?.Dispose();
            m_Worker = null;
            m_ExecutionLock.Dispose();
        }

        void ThrowIfDisposed()
        {
            if (m_Disposed)
                throw new ObjectDisposedException(nameof(KokoroHandler));
        }

        public class Voice: IDisposable
        {
            public string Name;
            public Tensor<float> Tensor;

            public Voice(string name, Tensor<float> data)
            {
                Name = name;
                Tensor = data;
            }
            public void Dispose()
            {
                Tensor?.Dispose();
                Tensor = null;
            }
        }
    }
}
