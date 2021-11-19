namespace ResearchXBRL.Application.Usecase.AccountElements.Transfer
{
    public interface ITransferAccountElementsPresenter
    {
        void Progress(int percentage);
        void Complete();
    }
}
