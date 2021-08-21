﻿using Artemis.Core;
using Artemis.Core.Modules;
using Stylet;

namespace Artemis.VisualScripting.Nodes.CustomViewModels
{
    public class DataModelNodeCustomViewModel : CustomNodeViewModel
    {
        private readonly DataModelNode _node;
        private BindableCollection<Module> _modules;

        public DataModelNodeCustomViewModel(DataModelNode node) : base(node)
        {
            _node = node;
        }

        public BindableCollection<Module> Modules
        {
            get => _modules;
            set => SetAndNotify(ref _modules, value);
        }

        public DataModelPath DataModelPath
        {
            get => _node.DataModelPath;
            set
            {
                _node.DataModelPath = value;
                OnPropertyChanged(nameof(DataModelPath));

                if (_node.DataModelPath != null)
                {
                    _node.DataModelPath.Save();
                    _node.Storage = _node.DataModelPath.Entity;
                }
                else
                {
                    _node.Storage = null;
                }

                _node.UpdateOutputPin();
            }
        }

        public void Initialize()
        {
            if (Modules != null)
                return;

            Modules = new BindableCollection<Module>();
            if (_node.Script.Context is Profile scriptProfile && scriptProfile.Configuration.Module != null)
                Modules.Add(scriptProfile.Configuration.Module);
            else if (_node.Script.Context is ProfileConfiguration profileConfiguration && profileConfiguration.Module != null)
                Modules.Add(profileConfiguration.Module);
        }

        #region Overrides of CustomNodeViewModel

        /// <inheritdoc />
        protected override void OnDisplay()
        {
            Initialize();
            base.OnDisplay();
        }

        #endregion
    }
}