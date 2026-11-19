using UnityEngine;
using UnityEngine.UI;
using System;
using Newtonsoft.Json;

namespace FalmeStreamless.Credits
{
	[RequireComponent(typeof(CanvasScaler))]
    public class Credits : MonoBehaviour
    {
        public static event Action OnCreditsFinished;

        [Header("All Credits Data")]
        [SerializeField] private TextAsset creditsJSON;

        [Header("References")]
        [SerializeReference] private Scroll scroll;

        void OnEnable()
        {
            End.onCreditEndReached += CreditEndReached;
        }

        void OnDisable()
        {
            End.onCreditEndReached -= CreditEndReached;
        }

        void Start()
        {
            CanvasScaler canvasScaler = GetComponent<CanvasScaler>();

            scroll.Initialize(
                canvasScaler.referenceResolution,
                GetJsonData()
                );
        }

        private CreditsData GetJsonData()
        {
            return JsonConvert.DeserializeObject<CreditsData>(creditsJSON.text);
        }

        void CreditEndReached(float difference)
        {
            scroll.StopScrolling();
            scroll.ScrollAdd(-difference); // Fix Overshot position
            OnCreditsFinished?.Invoke();
        }
    }
}
