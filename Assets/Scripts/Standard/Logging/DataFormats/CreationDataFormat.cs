using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathNav.ExperimentControl
{
    using System;

    public class CreationDataFormat 
    {
        public int    ID       { get; set; }	
        public int    BLOCK_ID { get; set; }	
        public string MODEL    { get; set; }
        public string METHOD   { get; set; }	
        public string ACTION   { get; set; }
        public DateTime TIMESTAMP   { get; set; }
    }
}
