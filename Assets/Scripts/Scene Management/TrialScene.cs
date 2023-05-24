using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathNav.ExperimentControl
{
    using Events;
    using Extensions;
    using Interaction;
    using PathPlanning;
    using Patterns.Factory;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEngine.AddressableAssets;

    public class TrialScene : MonoBehaviour
    {
        [SerializeField] private PathStrategy pathStrategy;

        private Model _model;
        private GameObject _trialModel;

        private void Enable()
        {
            DeclarePathStrategy();
        }

        private void DeclarePathStrategy()
        {
            EventManager.Publish(EventId.SetPathStrategy, this, new CreationTrialEventArgs(pathStrategy));
        }

        private async void CreateModel()
        {
           bool result = await PlaceModelAsync();
           
           if(!result) 
               Debug.LogError("Model Failed To Load! Unable to continue.");
        }
        
        private async Task<bool> PlaceModelAsync()
        {
            Task<GameObject> modelTask = InstantiateModelAsync(_model.assetReference);
            GameObject       model     = await modelTask;

            if (model is null) return false;

            _trialModel                      = model;
            _trialModel.transform.position   = _model.Translation;
            _trialModel.transform.localScale = _model.Scale * Vector3.one;

            return true;
        }

        private async Task<GameObject> InstantiateModelAsync(AssetReferenceGameObject key)
        {
            Task<GameObject> modelTask = Factory.InstantiateObjectAsync(key, Utility.Parameterize(transform), CancellationToken.None);
            GameObject       model     = await modelTask;

            if (!modelTask.IsCompletedSuccessfully) return null;
            if (model == null) return null;

            model.transform.SetParent(null); 
            model.AddComponent<ReleaseAddressable>();

            return model;
        }
    }

    public class CreationTrialEventArgs : EventArgs
    {
        public CreationTrialEventArgs(PathStrategy strategy) => Strategy = strategy;

        public PathStrategy Strategy { get; }
    }
}