using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JoeGatling.ButtonGrids.ButtonHandlers
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class HandlerName : Attribute
    {
        private string _name;
        public string name => _name;

        public HandlerName(string name)
        {
            _name = name;
        }
    }

}