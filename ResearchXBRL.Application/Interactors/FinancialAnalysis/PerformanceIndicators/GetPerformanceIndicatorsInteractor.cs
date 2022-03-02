using System;
using System.Threading.Tasks;
using ResearchXBRL.Application.DTO.FinancialAnalysis.PerformanceIndicators;
using ResearchXBRL.Application.Usecase.FinancialAnalysis.PerformanceIndicators;
using ResearchXBRL.Domain.FinancialAnalysis.PerformanceIndicators;
using ResearchXBRL.Domain.FinancialAnalysis.PerformanceIndicators.Corporations;

namespace ResearchXBRL.Application.Interactors.FinancialAnalysis.PerformanceIndicators;

public sealed class GetPerformanceIndicatorsInteractor : IGetPerformanceIndicatorsUsecase
{
    private readonly IPerformanceIndicatorsRepository repository;
    private readonly ICorporationsRepository corporationRepository;

    public GetPerformanceIndicatorsInteractor(
        IPerformanceIndicatorsRepository repository,
        ICorporationsRepository corporationRepository)
    {
        this.repository = repository;
        this.corporationRepository = corporationRepository;
    }

    public async ValueTask<PerformanceIndicatorsViewModel> Handle(string corporationId)
    {
        if (!await corporationRepository.Exists(corporationId))
        {
            throw new ArgumentException("指定された企業は存在しません");
        }

        var performanceIndicators = await repository.Get(corporationId);

        return new PerformanceIndicatorsViewModel(performanceIndicators);
    }
}
