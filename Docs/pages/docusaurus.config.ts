import {themes as prismThemes} from 'prism-react-renderer';
import type {Config} from '@docusaurus/types';
import type * as Preset from '@docusaurus/preset-classic';

const config: Config = {
  title: 'Testably.Abstractions',
  tagline: 'Feature-complete testing helpers for IFileSystem, ITimeSystem and IRandomSystem',
  favicon: 'img/favicon.ico',

  future: {
    v4: true,
  },

  url: 'https://docs.testably.org',
  baseUrl: '/',

  organizationName: 'Testably',
  projectName: 'Testably.Abstractions',

  onBrokenLinks: 'throw',

  i18n: {
    defaultLocale: 'en',
    locales: ['en'],
  },

  presets: [
    [
      'classic',
      {
        docs: {
          sidebarPath: './sidebars.ts',
          editUrl:
            'https://github.com/Testably/Testably.Abstractions/tree/main/Docs/pages/',
        },
        blog: false,
        theme: {
          customCss: './src/css/custom.css',
        },
      } satisfies Preset.Options,
    ],
  ],

  themeConfig: {
    image: 'img/social-preview.png',
    colorMode: {
      respectPrefersColorScheme: true,
    },
    navbar: {
      title: 'Testably.Abstractions',
      logo: {
        alt: 'Testably.Abstractions logo',
        src: 'img/logo.png',
      },
      items: [
        {
          type: 'docSidebar',
          sidebarId: 'docsSidebar',
          position: 'left',
          label: 'Docs',
        },
        {
          href: 'https://www.nuget.org/packages/Testably.Abstractions',
          label: 'NuGet',
          position: 'right',
        },
        {
          href: 'https://github.com/Testably/Testably.Abstractions',
          label: 'GitHub',
          position: 'right',
        },
      ],
    },
    footer: {
      style: 'dark',
      links: [
        {
          title: 'Docs',
          items: [
            {label: 'Introduction', to: '/docs/intro'},
            {label: 'Getting Started', to: '/docs/getting-started'},
            {label: 'Examples', to: '/docs/examples'},
          ],
        },
        {
          title: 'Packages',
          items: [
            {
              label: 'Testably.Abstractions',
              href: 'https://www.nuget.org/packages/Testably.Abstractions',
            },
            {
              label: 'Testably.Abstractions.Testing',
              href: 'https://www.nuget.org/packages/Testably.Abstractions.Testing',
            },
            {
              label: 'Testably.Abstractions.Compression',
              href: 'https://www.nuget.org/packages/Testably.Abstractions.Compression',
            },
            {
              label: 'Testably.Abstractions.AccessControl',
              href: 'https://www.nuget.org/packages/Testably.Abstractions.AccessControl',
            },
          ],
        },
        {
          title: 'More',
          items: [
            {
              label: 'GitHub',
              href: 'https://github.com/Testably/Testably.Abstractions',
            },
            {
              label: 'Issue tracker',
              href: 'https://github.com/Testably/Testably.Abstractions/issues',
            },
          ],
        },
      ],
      copyright: `Copyright © ${new Date().getFullYear()} Testably. Built with Docusaurus.`,
    },
    prism: {
      theme: prismThemes.github,
      darkTheme: prismThemes.dracula,
      additionalLanguages: ['csharp', 'powershell', 'bash'],
    },
  } satisfies Preset.ThemeConfig,
};

export default config;
