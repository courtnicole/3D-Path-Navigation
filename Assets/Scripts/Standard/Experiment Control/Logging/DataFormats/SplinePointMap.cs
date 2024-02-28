namespace PathNav.ExperimentControl
{
    using CsvHelper.Configuration;
    using Dreamteck.Splines;

    public sealed class SplinePointMap : ClassMap<SplinePoint>
    {
        
        public SplinePointMap()
        {
            Map(m => m.position.x).Index(0).Name("PositionX");
            Map(m => m.position.y).Index(1).Name("PositionY");
            Map(m => m.position.z).Index(2).Name("PositionZ");
            
            Map(m => m.tangent.x).Index(3).Name("TangentX");
            Map(m => m.tangent.y).Index(4).Name("TangentY");
            Map(m => m.tangent.z).Index(5).Name("TangentZ");
            
            Map(m => m.tangent2.x).Index(6).Name("Tangent2X");
            Map(m => m.tangent2.y).Index(7).Name("Tangent2Y");
            Map(m => m.tangent2.z).Index(8).Name("Tangent2Z");
            
            Map(m => m.normal.x).Index(9).Name("NormalX");
            Map(m => m.normal.y).Index(10).Name("NormalY");
            Map(m => m.normal.z).Index(11).Name("NormalZ");
            
            Map(m => m.size).Index(12).Name("Size");
        }
    }
}
