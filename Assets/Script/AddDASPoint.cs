using Export.AddFunc;
using Export.Attribute;
using Export.BehaviourEX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace Export.Tools
{
    /// <summary>
    /// 添加DropAndStick的点
    /// </summary>
    class AddDASPoint: UUIDBehavior
    {
        /// <summary>
        /// 追加点位名称
        /// </summary>
        [SerializeField]
        public string addPointsName;

        /// <summary>
        /// 拖拽脚本
        /// (用于判断点是否添加到点表中)
        /// </summary>
        public DragAndStick dragScript;

        /// <summary>
        /// 读取名称
        /// </summary>
        private Regex readName;

        /// <summary>
        /// 追加点
        /// </summary>
        [SerializeField]
        [ReadOnly]
        public Transform[] points;

        /// <summary>
        /// 追加点
        /// </summary>
        public List<Transform> Points
        {
            get
            {
                return new List<Transform>(points);
            }
        }

        private void Start()
        {
            readName = new Regex(addPointsName);

            points = transform.GetComponentsInChildren<Transform>();
            points = points.Where(t => t != null && readName.IsMatch(t.name)).ToArray();

            if (dragScript == null)
            {
                DragAndStickBehaviour._points.Add(UUID,Points);
            }
        }

        private void Update()
        {
            if (dragScript!=null)
            {
                if (dragScript.isSticked)
                {
                    DragAndStickBehaviour._points.AddOrSet(UUID,Points);
                }
                else
                {
                    DragAndStickBehaviour._points.Remove(UUID);
                }
            }
        }
    }
}
