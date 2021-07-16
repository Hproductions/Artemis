﻿using System.Linq;
using Artemis.VisualScripting.Attributes;
using Artemis.VisualScripting.Model;

namespace Artemis.VisualScripting.Nodes
{
    [UI("Sum (Integer)", "Sums the connected integer values.")]
    public class SumIntegersNode : Node
    {
        #region Properties & Fields

        public InputPinCollection<int> Values { get; }

        public OutputPin<int> Sum { get; }

        #endregion

        #region Constructors

        public SumIntegersNode()
            : base("Sum", "Sums the connected integer values.")
        {
            Values = CreateInputPinCollection<int>("Values", 2);
            Sum = CreateOutputPin<int>("Sum");
        }

        #endregion

        #region Methods

        public override void Evaluate()
        {
            Sum.Value = Values.Values.Sum();
        }

        #endregion
    }

    [UI("Sum (Double)", "Sums the connected double values.")]
    public class SumDoublesNode : Node
    {
        #region Properties & Fields

        public InputPinCollection<double> Values { get; }

        public OutputPin<double> Sum { get; }

        #endregion

        #region Constructors

        public SumDoublesNode()
            : base("Sum", "Sums the connected double values.")
        {
            Values = CreateInputPinCollection<double>("Values", 2);
            Sum = CreateOutputPin<double>("Sum");
        }

        #endregion

        #region Methods

        public override void Evaluate()
        {
            Sum.Value = Values.Values.Sum();
        }

        #endregion
    }
}