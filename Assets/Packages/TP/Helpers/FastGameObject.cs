using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace TP.Helpers
{
    public class FastGameObject
    {
        private static Dictionary<string, GameObject> tagObjectDict = new Dictionary<string, GameObject> { };

        public static GameObject FindGameObjectWithTag(string tag)
        {
            GameObject gameObject;
            bool found = tagObjectDict.TryGetValue(tag, out gameObject);
            
            if (!found)
            {
                gameObject = GameObject.FindGameObjectWithTag(tag);
                tagObjectDict.Add(tag, gameObject);
            }
            else if (gameObject == null)
            {
                tagObjectDict.Remove(tag);
                gameObject = FindGameObjectWithTag(tag);
            }

            return gameObject;
        }
                
        static int flushRate = 30;
        
        private static Dictionary<Type, Object[]> typeObjectsDict = new Dictionary<Type, Object[]> { };

        public static T[] FindObjectsOfType<T>() where T : Object
        {
            T[] objects = new T[] { };
            Type type = typeof(T);

            Object[] dictObjects;
            bool found = typeObjectsDict.TryGetValue(type, out dictObjects);
            if (found)
            {
                if (dictObjects != null)
                {
                    objects = Array.ConvertAll(dictObjects, item => (T)item);
                }
                
                if (Random.Range(0, flushRate) == flushRate)
                {
                    typeObjectsDict.Remove(type);
                    found = false;
                }
            }

            if (!found)
            {
                objects = GameObject.FindObjectsOfType<T>();
                typeObjectsDict.Add(type, objects);
            }

            return objects;
        }
    }
        
    public static class Extensions
    {
        //Possibly want to add extensions methods for GameObject that call the FastGameObject methods   . 
    }
}
