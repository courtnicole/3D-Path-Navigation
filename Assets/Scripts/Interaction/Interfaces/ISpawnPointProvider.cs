namespace PathNav.Interaction
{
    public interface ISpawnPointProvider
    {
        ISpawnPoint SpawnPointCandidate { get; }
        ISpawnPoint SpawnPoint { get; }

        bool HasSpawnCandidate => SpawnPointCandidate != null;
        bool HasSpawnPoint => SpawnPoint              != null;

        ISpawnPoint ComputeSpawnPoint();

        void AcquireSpawnPoint();
        void ClearSpawnPoint();
    }
}
