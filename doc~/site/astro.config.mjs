// @ts-check
import { defineConfig } from 'astro/config';
import starlight from '@astrojs/starlight';

// https://astro.build/config
export default defineConfig({
	integrations: [
		starlight({
			title: 'Avatar Menu Creator for MA',
			logo: {
				src: './public/favicon.svg',
			},
			head: [
				{
					tag: "script",
					attrs: {
						async: true,
						src: "https://www.googletagmanager.com/gtag/js?id=G-YKE861NPYC",
					}
				},
				{
					tag: "script",
					attrs: {
						src: "/ga.js",
					}
				},
				{
					tag: "meta",
					attrs: {
						property: "og:image",
						content: "https://avatar-menu-creator-for-ma.vrchat.narazaka.net/AvatarMenuCreator.png",
					}
				}
			],
			customCss: [
				'./src/styles/custom.css',
			],
			locales: {
				root: {
					label: '日本語',
					lang: 'ja',
				}
			},
			social: [{
				icon: 'github',
				label: 'GitHub',
				href: 'https://github.com/Narazaka/AvatarMenuCreaterForMA',
			}],
			editLink: {
				baseUrl: 'https://github.com/Narazaka/AvatarMenuCreaterForMA/edit/master/doc~/site',
			},
			sidebar: [
				{
					label: 'ガイド',
					autogenerate: { directory: 'guides' },
				},
				{
					label: '詳しい使い方',
					autogenerate: { directory: 'usecases' },
				},
				{
					label: 'リファレンス',
					autogenerate: { directory: 'references' },
				},
			],
		}),
	],
});
