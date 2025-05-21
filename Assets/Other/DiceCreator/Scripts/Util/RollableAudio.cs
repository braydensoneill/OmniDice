using System;
using UnityEngine;

namespace InnerDriveStudios.DiceCreator
{
    /**
     * Scaffold for an audio script that you can add to any rollable.
     * It is unfinished, but provides a starting point.
     * 
     * @author J.C. Wichman
     * @copyright Inner Drive Studios
     */
    [RequireComponent(typeof(AudioSource))]
    public class RollableAudio : MonoBehaviour
    {
        [SerializeField]
        private ARollable _rollable;

        private AudioSource _audioSource;

        void Awake()
        {
            _audioSource = GetComponent<AudioSource>();

            if (_rollable == null) _rollable = GetComponent<ARollable>();
            if (_rollable == null)
            {
                Debug.Log("RollableAudio requires a reference to ARollable instance");
            } else
            {
                _rollable.OnRollBegin += onRollBegin;
                _rollable.OnRollEnd += onRollEnd;
            }
        }

        private void onRollBegin(ARollable obj)
        {
            //play sound here? 
            _audioSource.Play();
        }

        private void onRollEnd(ARollable obj)
        {
            //stop sound here?
            _audioSource.Stop();
        }

        private void OnCollisionEnter(Collision collision)
        {
            //play sound on collision?
        }

    }
}