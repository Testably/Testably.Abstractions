import type {ReactNode} from 'react';
import clsx from 'clsx';
import Link from '@docusaurus/Link';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import Layout from '@theme/Layout';
import Tabs from '@theme/Tabs';
import TabItem from '@theme/TabItem';
import CodeBlock from '@theme/CodeBlock';
import Heading from '@theme/Heading';
import styles from './index.module.css';

const fileSystemProd =`public class ReportService(IFileSystem fileSystem)
{
    public void Save(string content)
    {
        fileSystem.Directory.CreateDirectory("reports");
        fileSystem.File.WriteAllText("reports/latest.xml", content);
    }
}`;

const fileSystemTest = `[Fact]
public async Task Save_WritesReportToReportsFolder()
{
    var fileSystem = new MockFileSystem();
    var sut = new ReportService(fileSystem);

    sut.Save("<report />");

    await Expect.That(fileSystem.File.ReadAllText("reports/latest.xml"))
        .IsEqualTo("<report />");
}`;

const timeSystemProd = `public class CacheEntry(ITimeSystem timeSystem, TimeSpan ttl)
{
    private readonly DateTime _expiresAt = timeSystem.DateTime.UtcNow + ttl;

    public bool IsExpired => timeSystem.DateTime.UtcNow >= _expiresAt;
}`;

const timeSystemTest = `[Fact]
public async Task IsExpired_ReturnsTrue_AfterTtlPasses()
{
    MockTimeSystem timeSystem = new(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    var entry = new CacheEntry(timeSystem, TimeSpan.FromMinutes(5));

    await timeSystem.Task.Delay(TimeSpan.FromMinutes(6));

    await Expect.That(entry.IsExpired).IsTrue();
}`;

const randomSystemProd = `public class CorrelationIdProvider(IRandomSystem randomSystem)
{
    public string Next() => randomSystem.Guid.NewGuid().ToString();
}`;

const randomSystemTest = `[Fact]
public async Task Next_ReturnsConfiguredGuid_ForDeterministicTests()
{
    MockRandomSystem randomSystem = new(RandomProvider.Generate(
        guidGenerator: () => Guid.Parse("11111111-1111-1111-1111-111111111111")));
    var sut = new CorrelationIdProvider(randomSystem);

    await Expect.That(sut.Next())
        .IsEqualTo("11111111-1111-1111-1111-111111111111");
}`;

function HomepageHeader() {
  const {siteConfig} = useDocusaurusContext();
  return (
    <header className={clsx('hero hero--primary', styles.heroBanner)}>
      <div className="container">
        <Heading as="h1" className="hero__title">
          {siteConfig.title}
        </Heading>
        <p className="hero__subtitle">{siteConfig.tagline}</p>
        <div className={styles.buttons}>
          <Link
            className="button button--secondary button--lg"
            to="/docs/getting-started">
            Get started
          </Link>
          <Link
            className={clsx(
              'button button--outline button--secondary button--lg',
              styles.secondaryButton,
            )}
            to="/docs/intro">
            Learn more
          </Link>
        </div>
      </div>
    </header>
  );
}

function HomepageCodeSample() {
  return (
    <section className={styles.codeSample}>
      <div className="container">
        <Heading as="h2" className="text--center">
          Inject. Mock. Test.
        </Heading>
        <p className={clsx('text--center', styles.codeSampleSubtitle)}>
          Depend on <code>IFileSystem</code>, <code>ITimeSystem</code> and{' '}
          <code>IRandomSystem</code> in production. Swap in the in-memory mocks for
          tests - deterministic, cross-platform, no temp folders or{' '}
          <code>Thread.Sleep</code>.
        </p>
        <div className={styles.codeSampleContainer}>
          <Tabs groupId="codeSample" className={styles.codeSampleTabs}>
            <TabItem value="file" label="File system" default>
              <div className={styles.codeSampleStack}>
                <CodeBlock language="csharp" title="ReportService.cs">
                  {fileSystemProd}
                </CodeBlock>
                <CodeBlock language="csharp" title="ReportServiceTests.cs">
                  {fileSystemTest}
                </CodeBlock>
              </div>
            </TabItem>
            <TabItem value="time" label="Time system">
              <div className={styles.codeSampleStack}>
                <CodeBlock language="csharp" title="CacheEntry.cs">
                  {timeSystemProd}
                </CodeBlock>
                <CodeBlock language="csharp" title="CacheEntryTests.cs">
                  {timeSystemTest}
                </CodeBlock>
              </div>
            </TabItem>
            <TabItem value="random" label="Random system">
              <div className={styles.codeSampleStack}>
                <CodeBlock language="csharp" title="CorrelationIdProvider.cs">
                  {randomSystemProd}
                </CodeBlock>
                <CodeBlock language="csharp" title="CorrelationIdProviderTests.cs">
                  {randomSystemTest}
                </CodeBlock>
              </div>
            </TabItem>
          </Tabs>
        </div>
      </div>
    </section>
  );
}

export default function Home(): ReactNode {
  const {siteConfig} = useDocusaurusContext();
  return (
    <Layout
      title={siteConfig.title}
      description="Testably.Abstractions provides IFileSystem, ITimeSystem and IRandomSystem interfaces with a feature-complete in-memory MockFileSystem for unit tests.">
      <HomepageHeader />
      <main>
        <HomepageCodeSample />
      </main>
    </Layout>
  );
}
