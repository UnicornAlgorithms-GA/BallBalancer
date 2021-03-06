﻿using System;
using System.Linq;
using GeneticLib.Genome.NeuralGenomes;
using UnityEngine;

public abstract class AgentProxy : MonoBehaviour
{
	public NeuralGenome neuralGenome;
	protected PopulationProxy populationProxy;

	public int nbOfInputs;
	public int nbOfOutputs;

	public virtual void Init(PopulationProxy populationProxy)
	{
		this.populationProxy = populationProxy;
	}

	public virtual void ResetAgent(NeuralGenome newNeuralGenome = null)
	{
		if (newNeuralGenome != null)
		    newNeuralGenome.NetworkOperationBaker.BakeNetwork(newNeuralGenome);
		
		this.neuralGenome = newNeuralGenome;

		gameObject.SetActive(true);
		this.neuralGenome.Fitness = 0;
	}

	public virtual void End()
	{
		gameObject.SetActive(false);
	}

	#region Genetics
	public abstract void MoveFromNetwork();
	#endregion
}


public struct InitialAgentPartState
{
	public Vector3 localPos;
	public Quaternion localRot;
}