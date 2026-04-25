using System;
using System.Collections.Generic;
using System.Text;

namespace ImageAnalysis_Ollama.Models
{
    class TrafficCamResult
    {
        public TrafficStatus Status { get; set; }
        public int NumCars { get; set; }
        public int NumTrucks { get; set; }

        public enum TrafficStatus { Clear, Flowing, Congested, Blocked };
    }
}
