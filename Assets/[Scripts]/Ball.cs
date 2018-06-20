using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Ball : MonoBehaviour
{
	public UnityEvent onTriggerEnter;
   
	private void OnTriggerEnter(Collider other)
	{
		onTriggerEnter?.Invoke();
	}
}
