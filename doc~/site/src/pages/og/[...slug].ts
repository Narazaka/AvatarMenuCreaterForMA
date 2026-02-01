import { getCollection } from 'astro:content'
import { OGImageRoute } from 'astro-og-canvas'

// Get all entries from the `docs` content collection.
const entries = await getCollection('docs')

// Map the entry array to an object with the page ID as key and the
// frontmatter data as value.
const pages = Object.fromEntries(entries.map(({ data, id }) => [id, { data }]))

export const { getStaticPaths, GET } = await OGImageRoute({
  // Pass down the documentation pages.
  pages,
  // Define the name of the parameter used in the endpoint path, here `slug`
  // as the file is named `[...slug].ts`.
  param: 'slug',
  // Define a function called for each page to customize the generated image.
  getImageOptions: (_id, page: (typeof pages)[number]) => {
    return {
      // Use the page title and description as the image title and description.
      title: page.data.title,
      description: page.data.description,
        logo: {
        path: './src/assets/og/AvatarMenuCreatorLogo.png',
        },
        bgGradient: [[255, 255, 255], [255, 255, 255], [173, 245, 255]],
        font: {
            title: {
                color: [10, 10, 10],
            },
            description: {
                color: [10, 10, 10],
            },
        },
        fonts: ["./src/assets/og/rounded-x-mgenplus-1p-black.ttf"],
    }
  },
})