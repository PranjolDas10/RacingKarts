using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarEffects : MonoBehaviour
{
    private PlayerController controller;
    private InputManager IM;

    public ParticleSystem[] nitrusSmoke;
    // Start is called before the first frame update
    void Start()
    {
        if (gameObject.tag == "AI") return;
        controller = GetComponent<PlayerController>();
        IM = GetComponent<InputManager>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (gameObject.tag == "AI") return;
    }

    public void startNitrusEmitter()
    {
        if (controller.nitrusFlag) return;
        for (int i = 0; i < nitrusSmoke.Length; i++)
        {
            nitrusSmoke[i].Play();
        }

        controller.nitrusFlag = true;
    }
    public void stopNitrusEmitter()
    {
        if (!controller.nitrusFlag) return;
        for (int i = 0; i < nitrusSmoke.Length; i++)
        {
            nitrusSmoke[i].Stop();
        }
        controller.nitrusFlag = false;

    }
}
