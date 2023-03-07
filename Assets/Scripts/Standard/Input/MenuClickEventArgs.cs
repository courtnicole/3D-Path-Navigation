namespace PathNav.Input
{
	using System;
	using Interaction;

	public class MenuClickEventArgs : EventArgs
	{
		public MenuClickEventArgs(IInteractable interactable, IController controller)
        {
            Interactable = interactable;
            Controller   = controller;
        }

        public MenuClickEventArgs(RaycastEvaluatorEventArgs args)
        {
            Interactable = args.Interactable;
            Controller   = args.Controller;
        }

		public IController Controller { get; }
        public IInteractable Interactable { get; }
	}
}