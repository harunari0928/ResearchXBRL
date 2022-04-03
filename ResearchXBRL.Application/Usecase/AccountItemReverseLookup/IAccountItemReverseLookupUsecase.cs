using System.Threading.Tasks;

namespace ResearchXBRL.Application.Usecase.AccountItemReverseLookup;

public interface IAccountItemReverseLookupUsecase
{
    /// <summary>
    /// 財務諸表情報からXBRL会計項目名を逆引きし、リポジトリに保存する
    /// </summary>
    /// <returns></returns>
    ValueTask Handle();
}
