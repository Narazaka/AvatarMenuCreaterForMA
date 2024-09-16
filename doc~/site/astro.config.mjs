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
			customCss: [
				'./src/styles/custom.css',
			],
			locales: {
				root: {
					label: '日本語',
					lang: 'ja',
				}
			},
			social: {
				github: 'https://github.com/Narazaka/AvatarMenuCreaterForMA',
			},
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
			],
		}),
	],
});
