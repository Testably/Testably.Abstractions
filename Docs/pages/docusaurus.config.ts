import {themes as prismThemes} from 'prism-react-renderer';
import type {Config} from '@docusaurus/types';
import type * as Preset from '@docusaurus/preset-classic';

const config: Config = {
  title: 'Testably.Abstractions',
  tagline: 'Mock the unmockable.',
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
      ],
    },
    footer: {
      style: 'dark',
      links: [
        {
          title: 'Docs',
          items: [
            {label: 'File system', to: '/docs/file-system/'},
            {label: 'Time system', to: '/docs/time-system/'},
            {label: 'Random system', to: '/docs/random-system/'},
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
              label: 'NuGet',
              href: 'https://www.nuget.org/packages/Testably.Abstractions',
            },
          ],
        },
      ],
      copyright: `Copyright © ${new Date().getFullYear()} Testably.`,
    },
    prism: {
      theme: prismThemes.github,
      darkTheme: prismThemes.dracula,
      additionalLanguages: ['csharp', 'powershell', 'bash'],
    },
  } satisfies Preset.ThemeConfig,
};

export default config;
