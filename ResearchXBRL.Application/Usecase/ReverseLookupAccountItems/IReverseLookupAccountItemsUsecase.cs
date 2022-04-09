using System.Threading.Tasks;

namespace ResearchXBRL.Application.Usecase.ReverseLookupAccountItems;

public interface IReverseLookupAccountItemsUsecase
{
    /// <summary>
    /// 財務諸表情報からXBRL会計項目名を逆引きし、リポジトリに保存する
    /// </summary>
    /// <returns></returns>
    ValueTask Handle();
}
