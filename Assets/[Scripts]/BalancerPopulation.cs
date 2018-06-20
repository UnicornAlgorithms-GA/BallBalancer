using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GeneticLib.Neurology;
using GeneticLib.Neurology.NeuralModels;
using GeneticLib.Neurology.Neurons;
using GeneticLib.Randomness;
using UnityEngine;

public class BalancerPopulation : PopulationProxy
{
    [Header("Agent initialization")]
    public Transform lowerInitLimit;
    public float distBetweenAgents = 5;
    public int agentsPerRow = 10;

	[Header("Random force")]
	public float randomForce = 1f;
	public float randomForceInterval = 5;
	private float lastTimeRndForceApplied = 0;

    private void Start()
    {
        Init();
    }

    private void Update()
    {
        //if (!agents.Any(x => (x as PlatformAgent).ball.gameObject.activeSelf))
        //Evolve();
        if (!agents.Any(x => x.gameObject.activeSelf))
            Evolve();
    }

    private void FixedUpdate()
    {
        OnFixedUpdate();

		if (Time.time > lastTimeRndForceApplied + randomForceInterval)
		{
			foreach (var agent in agents)
			{
				if (agent.gameObject.activeSelf)
				{
					(agent as PlatformAgent).ball.AddForce(
						GARandomManager.NextFloat(-randomForce, randomForce),
                        0,
						GARandomManager.NextFloat(-randomForce, randomForce)
					);
				}
			}

			lastTimeRndForceApplied = Time.time;
		}
    }

	protected override void Evolve()
	{
		base.Evolve();
		lastTimeRndForceApplied = Time.time;
	}

	protected override INeuralModel InitNeuralModel()
    {
        var model = new NeuralModelBase();
        model.defaultWeightInitializer = () => GARandomManager.NextFloat(-1, 1);

        model.WeightConstraints = new Tuple<float, float>(
            weightConstraints.x,
            weightConstraints.y
        );

		var bias = model.AddBiasNeuron();

        var layers = new List<Neuron[]>()
        {
            // Inputs
            model.AddInputNeurons(
                agentPrefab.GetComponent<AgentProxy>().nbOfInputs
            ).ToArray(),

            model.AddNeurons(
                new Neuron(-1, ActivationFunctions.TanH),
                count: 3
            ).ToArray(),

            // Outputs
            model.AddOutputNeurons(
                agentPrefab.GetComponent<AgentProxy>().nbOfOutputs,
                ActivationFunctions.Sigmoid
            ).ToArray(),
        };

        model.ConnectLayers(layers);
		model.ConnectBias(bias, layers.Skip(1));

        return model;
    }

    protected override IEnumerable<AgentProxy> InitAgents()
    {
        for (int i = 0; i < genomesCount; i++)
        {
            var agent = Instantiate(agentPrefab, transform).GetComponent<AgentProxy>();
            agent.transform.position = new Vector3(
                lowerInitLimit.position.x + (i % agentsPerRow) * distBetweenAgents,
                lowerInitLimit.position.y + (i / agentsPerRow) * distBetweenAgents,
                0);
            agent.Init(this);
            yield return agent;
        }
    }
}
