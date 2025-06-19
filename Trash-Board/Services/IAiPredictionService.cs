namespace TrashBoard.Services
{
    public interface IAiPredictionService
    {
        Task<Stream?> GetDecisionTreePngAsync();
        Task<PredictionAmountResult?> GetForecastOfDayAsync(TrashDetectionInput input);
        Task<string?> PredictAsync(TrashDetectionInput input);
        Task<TrainingResult?> RetrainModelAsync(TrainingParameters parameters);
    }
}