﻿using System;
using Artemis.Storage.Entities.Profile.Nodes;

namespace Artemis.Core
{
    public class NodeData
    {
        #region Properties & Fields

        public Plugin Plugin { get; }

        public Type Type { get; }
        public string Name { get; }
        public string Description { get; }
        public string Category { get; }
        
        private Func<INodeScript, NodeEntity?, INode> _create;

        #endregion

        #region Constructors

        internal NodeData(Plugin plugin, Type type, string name, string description, string category, Func<INodeScript, NodeEntity?, INode>? create)
        {
            this.Plugin = plugin;
            this.Type = type;
            this.Name = name;
            this.Description = description;
            this.Category = category;
            this._create = create;
        }

        #endregion

        #region Methods

        public INode CreateNode(INodeScript script, NodeEntity? entity) => _create(script, entity);

        #endregion
    }
}