/*
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
using UnityEngine.EventSystems;

namespace Netherlands3D.SelectionTools
{
    public class PolygonDragHandle : MonoBehaviour, IDragHandler, IPointerClickHandler, IEndDragHandler
    {
        public UnityEvent dragged = new UnityEvent();
        public UnityEvent endDrag = new UnityEvent();
        public UnityEvent clicked = new UnityEvent();

        public int pointIndex = 0;

        [SerializeField] private float scale = 0.02f;
        [SerializeField] private float minScale = 2f;
        [SerializeField] private bool autoScale = true;
        private Camera camera;
        private void OnEnable()
        {
            camera = Camera.main;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            clicked.Invoke();
        }

        public void OnDrag(PointerEventData eventData)
        {
            dragged.Invoke();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            endDrag.Invoke();
        }

        private void Update()
        {
            var handleScale = Mathf.Max(minScale,scale * Vector3.Distance(camera.transform.position, transform.position));
            this.transform.localScale = Vector3.one * handleScale;
        }

        private void OnDestroy()
        {
            dragged.RemoveAllListeners();
            clicked.RemoveAllListeners();
        }
    }
}