namespace PathNav.ExperimentControl
{
    using CsvHelper.Configuration;

    public sealed class SceneDataFormatMap : ClassMap<SceneDataFormat>
    {
        public SceneDataFormatMap()
        {
            Map(m => m.ID).Index(0).Name("ID");
            Map(m => m.BLOCK_ID).Index(1).Name("BLOCK_ID");
            Map(m => m.TRIAL_TYPE).Index(2).Name("TRIAL_TYPE");
            Map(m => m.TRIAL_ID).Index(3).Name("TRIAL_ID");
            Map(m => m.SCENE_ID).Index(4).Name("SCENE_ID");
            Map(m => m.METHOD).Index(5).Name("METHOD");
            Map(m => m.MODEL).Index(6).Name("MODEL");
            Map(m => m.ACTIONS_TOTAL).Index(7).Name("ACTIONS_TOTAL");
            Map(m => m.ACTIONS_EDIT).Index(8).Name("ACTIONS_EDIT");
            Map(m => m.TASK_TIME_TOTAL).Index(9).Name("TASK_TIME_TOTAL");
            Map(m => m.TASK_TIME_EDIT).Index(10).Name("TASK_TIME_EDIT");
            Map(m => m.TASK_TIME_CREATE).Index(11).Name("TASK_TIME_CREATE");
            Map(m => m.SEQ_SCORE).Index(12).Name("SEQ_SCORE");
            Map(m => m.DISCOMFORT_SCORE).Index(13).Name("DISCOMFORT_SCORE");
        }
    }
}