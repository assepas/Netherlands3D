﻿/*
*  Copyright (C) X Gemeente
*                X Amsterdam
*                X Economic Services Departments
*
*  Licensed under the EUPL, Version 1.2 or later (the "License");
*  You may not use this work except in compliance with the License.
*  You may obtain a copy of the License at:
*
*    https://github.com/Amsterdam/3DAmsterdam/blob/master/LICENSE.txt
*
*  Unless required by applicable law or agreed to in writing, software
*  distributed under the License is distributed on an "AS IS" basis,
*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
*  implied. See the License for the specific language governing
*  permissions and limitations under the License.
*/
using UnityEngine;
using UnityEngine.Events;

namespace Netherlands3D.Events
{
	[System.Serializable]
	public class FloatValueUnityEvent : UnityEvent<float> { }

	[CreateAssetMenu(fileName = "FloatEvent", menuName = "EventContainers/FloatEvent", order = 0)]
	[System.Serializable]
	public class FloatEvent : EventContainer<FloatValueUnityEvent>
	{
		public void Invoke(float floatContent)
		{
			started.Invoke(floatContent);
		}
	}
}