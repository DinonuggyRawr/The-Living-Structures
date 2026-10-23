using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TLS.UI
{
    /// <summary>
    /// Maps the five EXISTING Phase 1 scene landmarks to their real subject labs.
    /// Nothing is renamed or rebuilt. Other laboratories remain reachable through
    /// the Academy subject directory. No walkable interiors are claimed.
    /// </summary>
    public static class TLSPhase8Campus
    {
        public sealed class Hub
        {
            public readonly string SceneName, Title, Purpose, LabLabel;
            public Hub(string sceneName,string title,string purpose,string labLabel)
            { SceneName=sceneName; Title=title; Purpose=purpose; LabLabel=labLabel; }
        }
        public static readonly Hub[] Hubs = {
            new Hub("STACK LAB / ACTIVE","Linear Structures Center","Stacks: LIFO memory and complete C11 functions.","OPEN STACK LAB"),
            new Hub("LINKED LISTS / LATER","Pointer & Linked List Workshop","Pointers, allocation, linked nodes and recursion.","OPEN LINKED LIST LAB"),
            new Hub("RECURSION / LATER","Recursion Chamber","Base cases, active calls, return values and unwinding.","OPEN RECURSION LAB"),
            new Hub("TREES / LATER","Tree Observatory","Binary trees, BST deletion and traversal logic.","OPEN TREE / BST LAB"),
            new Hub("GRAPHS / LATER","Graph Exploration Center","Adjacency, BFS, DFS and C traversal practice.","OPEN GRAPH LAB")
        };
        public static int ResolveSceneName(string name)
        {
            if (string.IsNullOrEmpty(name)) return -1;
            for (int i=0;i<Hubs.Length;i++)
                if (string.Equals(Hubs[i].SceneName,name,StringComparison.Ordinal)) return i;
            return -1;
        }
        public static int ResolveAncestor(Transform transform)
        {
            while (transform != null)
            {
                int hub=ResolveSceneName(transform.name);
                if (hub>=0)return hub;
                transform=transform.parent;
            }
            return -1;
        }
        public static bool TrySelect(Camera camera,Vector3 screenPoint,out int hub)
        {
            hub=-1;
            if (camera==null) return false;
            RaycastHit hit;
            if (!Physics.Raycast(camera.ScreenPointToRay(screenPoint),out hit,1000f)) return false;
            hub=ResolveAncestor(hit.collider.transform);
            return hub>=0;
        }
        public static bool PointerIsOverUI()
        { return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(); }
    }
}
