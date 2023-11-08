using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Mineral", menuName = "Rock/Mineral", order = 0)]
public class Mineral : ScriptableObject 
{
    public float resistance = Mathf.Infinity;
    public bool isWall = false;

    public ParticleSystem breakParticle;
    public Color canvasColor; 
    public Gradient tweenGradient;
    public Material[] materials;

    [HideInInspector] public float punchScale = 0.1f;
    [HideInInspector] public float tweenDuration = 0.8f;
    [HideInInspector] public float tweenElasticity = 1.0f;
}
