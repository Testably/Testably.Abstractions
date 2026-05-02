import type {ReactNode} from 'react';
import clsx from 'clsx';
import Heading from '@theme/Heading';
import styles from './styles.module.css';

type FeatureItem = {
  title: string;
  emoji: string;
  description: ReactNode;
};

const FeatureList: FeatureItem[] = [
  {
    title: 'Drop-in abstractions',
    emoji: '🔌',
    description: (
      <>
        <code>IFileSystem</code>, <code>ITimeSystem</code> and{' '}
        <code>IRandomSystem</code> wrap the static APIs from <code>System.IO</code>,{' '}
        <code>System.Diagnostics</code>, <code>System.Threading</code> and{' '}
        <code>System</code> so they can be injected and replaced in tests.
      </>
    ),
  },
  {
    title: 'A real in-memory file system',
    emoji: '💾',
    description: (
      <>
        <code>MockFileSystem</code> behaves exactly like the real file system: every test
        runs against both, so the mock and reality stay in sync. Drives, quotas,{' '}
        <code>FileSystemWatcher</code> and <code>SafeFileHandle</code> are all supported.
      </>
    ),
  },
  {
    title: 'Cross-platform simulation',
    emoji: '🖥️',
    description: (
      <>
        Simulate Linux, macOS or Windows file systems on any host - case-sensitivity,
        path separators, drive letters and Unix file modes are all honoured, so you can
        test platform-specific behaviour from a single CI job.
      </>
    ),
  },
];

function Feature({title, emoji, description}: FeatureItem) {
  return (
    <div className={clsx('col col--4')}>
      <div className="text--center">
        <div className={styles.featureEmoji} role="img">{emoji}</div>
      </div>
      <div className="text--center padding-horiz--md">
        <Heading as="h3">{title}</Heading>
        <p>{description}</p>
      </div>
    </div>
  );
}

export default function HomepageFeatures(): ReactNode {
  return (
    <section className={styles.features}>
      <div className="container">
        <div className="row">
          {FeatureList.map((props, idx) => (
            <Feature key={idx} {...props} />
          ))}
        </div>
      </div>
    </section>
  );
}
