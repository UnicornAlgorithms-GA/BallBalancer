using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GeneticLib.Genome.NeuralGenomes;
using UnityEngine;

public class PlatformAgent : AgentProxy
{
	public Rigidbody ball;
	private Vector3 initialLocalBallPos;

	public Transform platform;
	private BoxCollider platformCollider;
	private Rigidbody platformRb;

    // Used to normalize the ball velocity
	public float normBallVelocity = 50;   
	public float maxPlatformAngle = 80;

	public float rotationDamp = 10;

	private float startTime;

	private void Awake()
	{
		initialLocalBallPos = ball.transform.localPosition;
		platformCollider = platform.GetComponent<BoxCollider>();
		platformRb = platform.GetComponent<Rigidbody>();
	}

	private void FixedUpdate()
	{
		if (neuralGenome == null || !ball.gameObject.activeSelf)
			return;

		MoveFromNetwork();

		neuralGenome.Fitness += ComputeFitness();

		//Debug.Log(ball.velocity.x);
	}

	public override void MoveFromNetwork()
	{      
		neuralGenome.FeedNeuralNetwork(GenerateNetworkInputs());

		var outputs = neuralGenome.Outputs
                                  .Select(x => x.Value)
                                  .ToArray();

		Debug.Assert(outputs.Length == nbOfOutputs);

		var desiredRot = Quaternion.Euler(
			Mathf.Lerp(-maxPlatformAngle, maxPlatformAngle, outputs[0]),
			0,
			Mathf.Lerp(-maxPlatformAngle, maxPlatformAngle, outputs[1])
		);

		platform.rotation = Quaternion.Lerp(
			platform.rotation,
			desiredRot,
			Time.fixedDeltaTime * rotationDamp);
	}

	private float[] GenerateNetworkInputs()
	{
		var result = new float[6];

		result[0] = ball.transform.localPosition.x / platformCollider.bounds.extents.x;
		result[1] = ball.transform.localPosition.z / platformCollider.bounds.extents.z;

		result[2] = ball.velocity.x / normBallVelocity;
		result[3] = ball.velocity.z / normBallVelocity;

		result[4] = Mathf.InverseLerp(
			-maxPlatformAngle,
			maxPlatformAngle,
			platform.transform.rotation.eulerAngles.x);
		result[5] = Mathf.InverseLerp(
            -maxPlatformAngle,
            maxPlatformAngle,
            platform.transform.rotation.eulerAngles.z);
		
		return result;
	}

	public override void ResetAgent(NeuralGenome newNeuralGenome = null)
    {
        base.ResetAgent(newNeuralGenome);

		ball.transform.localPosition = initialLocalBallPos;
		ball.rotation = Quaternion.Euler(Vector3.zero);
		ball.velocity = Vector3.zero;
		ball.angularVelocity = Vector3.zero;
		ball.Sleep();

		platform.localPosition = Vector3.zero;
		platform.rotation = Quaternion.Euler(Vector3.zero);      
		platformRb.velocity = Vector3.zero;
		platformRb.angularVelocity = Vector3.zero;
		platformRb.Sleep();

		platform.gameObject.SetActive(true);
		ball.gameObject.SetActive(true);

		startTime = Time.time;
    }

	public void OnBallLimitTouch()
	{
		//ball.gameObject.SetActive(false);
		gameObject.SetActive(false);
	}

	private float ComputeFitness()
	{
		return Time.time - startTime -
            (0.1f * Vector3.Distance(ball.transform.localPosition, platform.localPosition));
	}
}
