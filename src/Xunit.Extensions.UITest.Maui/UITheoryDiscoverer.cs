using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit.Extensions.UITest;

public class UITheoryDiscoverer : TheoryDiscoverer
{
	public UITheoryDiscoverer(IMessageSink diagnosticMessageSink)
		: base(diagnosticMessageSink)
	{
	}

	protected override IEnumerable<IXunitTestCase> CreateTestCasesForDataRow(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo theoryAttribute, object[] dataRow)
	{
		yield return new UITestCase(DiagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), testMethod, dataRow);
	}

	protected override IEnumerable<IXunitTestCase> CreateTestCasesForTheory(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo theoryAttribute)
	{
		yield return new UITheoryTestCase(DiagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), Sdk.TestMethodDisplayOptions.None, testMethod);
	}
}
