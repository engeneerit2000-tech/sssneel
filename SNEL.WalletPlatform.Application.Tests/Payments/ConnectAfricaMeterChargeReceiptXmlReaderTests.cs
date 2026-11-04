using SNEL.WalletPlatform.Application.Payments.ConnectAfrica;
using Xunit;

namespace SNEL.WalletPlatform.Application.Tests.Payments;

public sealed class ConnectAfricaMeterChargeReceiptXmlReaderTests
{
    [Fact]
    public void ReadFromVendXml_WhenTk44PrecedesDettesLabel_TreatsAsPreTaxLevy()
    {
        const string xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <suprima version="3">
              <thin-client>
                <vend type="response">
                  <success>1</success>
                  <token>
                    <tk41>659,92</tk41>
                    <tk90>R.1 Tri_BE</tk90>
                    <tk71>@ 91.21/kWh</tk71>
                    <tk44>-50,00</tk44>
                    <tk93>Dettes 1</tk93>
                    <tk43>273,50</tk43>
                    <tk92>TVA</tk92>
                    <tk73>@ 16.0%</tk73>
                    <tk43>17,09</tk43>
                    <tk92>Eclairage Public</tk92>
                    <tk73>@ 1.0%</tk73>
                    <tk93>Avance Sur Travaux</tk93>
                    <tk70>0,00</tk70>
                    <tk93>Dettes 1</tk93>
                    <tk70>0,00</tk70>
                    <tk70>0,00 CDF</tk70>
                  </token>
                </vend>
              </thin-client>
            </suprima>
            """;

        var receipt = ConnectAfricaMeterChargeReceiptXmlReader.ReadFromVendXml(xml);

        Assert.Single(receipt.PreTaxLevyAmounts);
        Assert.Equal("-50,00", receipt.PreTaxLevyAmounts[0]);
        Assert.Equal("Dettes 1", receipt.PreTaxLevyNames[0]);
        Assert.Equal("0,00", receipt.AdvanceOnWorks);
        Assert.Equal("0,00", receipt.Debts1);
        Assert.Equal("0,00 CDF", receipt.ArrearsTotal);
    }

    [Fact]
    public void ReadFromVendXml_WhenDettesAppearsBeforeAndAfterTax_SplitsPreTaxLevyAndArrears()
    {
        const string xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <suprima version="3">
              <thin-client>
                <vend type="response">
                  <success>1</success>
                  <token>
                    <tk41>1709,41</tk41>
                    <tk90>R.1 Tri_BE</tk90>
                    <tk71>@ 91.21/kWh</tk71>
                    <tk44>-100,00</tk44>
                    <tk93>Dettes 1</tk93>
                    <tk43>273,50</tk43>
                    <tk92>TVA</tk92>
                    <tk73>@ 16.0%</tk73>
                    <tk93>Avance Sur Travaux</tk93>
                    <tk70>0,00</tk70>
                    <tk93>Dettes 1</tk93>
                    <tk70>0,00</tk70>
                    <tk70>0,00 CDF</tk70>
                  </token>
                </vend>
              </thin-client>
            </suprima>
            """;

        var receipt = ConnectAfricaMeterChargeReceiptXmlReader.ReadFromVendXml(xml);

        Assert.Single(receipt.PreTaxLevyAmounts);
        Assert.Equal("-100,00", receipt.PreTaxLevyAmounts[0]);
        Assert.Equal("Dettes 1", receipt.PreTaxLevyNames[0]);
        Assert.Equal("0,00", receipt.AdvanceOnWorks);
        Assert.Equal("0,00", receipt.Debts1);
        Assert.Equal("0,00 CDF", receipt.ArrearsTotal);
    }

    [Fact]
    public void ReadFromVendXml_WhenDettesAppearsOnlyAfterTax_KeepsArrearsSectionOnly()
    {
        const string xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <suprima version="3">
              <thin-client>
                <vend type="response">
                  <success>1</success>
                  <token>
                    <tk41>1709,41</tk41>
                    <tk43>273,50</tk43>
                    <tk92>TVA</tk92>
                    <tk73>@ 16.0%</tk73>
                    <tk93>Avance Sur Travaux</tk93>
                    <tk70>0,00</tk70>
                    <tk93>Dettes 1</tk93>
                    <tk70>0,00</tk70>
                    <tk70>0,00 CDF</tk70>
                  </token>
                </vend>
              </thin-client>
            </suprima>
            """;

        var receipt = ConnectAfricaMeterChargeReceiptXmlReader.ReadFromVendXml(xml);

        Assert.Empty(receipt.PreTaxLevyAmounts);
        Assert.Equal("0,00", receipt.Debts1);
        Assert.Equal("0,00 CDF", receipt.ArrearsTotal);
    }

    [Fact]
    public void ReadFromVendXml_WhenTk44IsPositive_PreservesPositiveAmount()
    {
        const string xml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <suprima version="3">
              <thin-client>
                <vend type="response">
                  <success>1</success>
                  <token>
                    <tk44>50,00</tk44>
                    <tk93>Dettes 1</tk93>
                    <tk43>273,50</tk43>
                    <tk92>TVA</tk92>
                    <tk73>@ 16.0%</tk73>
                  </token>
                </vend>
              </thin-client>
            </suprima>
            """;

        var receipt = ConnectAfricaMeterChargeReceiptXmlReader.ReadFromVendXml(xml);

        Assert.Single(receipt.PreTaxLevyAmounts);
        Assert.Equal("50,00", receipt.PreTaxLevyAmounts[0]);
    }
}
