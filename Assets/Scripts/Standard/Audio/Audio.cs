namespace PathNav.SceneManagement
{
    [System.Serializable]
    public sealed class Audio 
    {
        public enum Id
        {
            PlaceStart,
            DrawingDraw,
            DrawingErase,
            InterpolatePlace,
            InterpolateMove,
            InterpolateDelete,
            FinishPath,
        }

        public enum Fx
        {
        }
    }
}
