const { join } = require("path");
const { createGlobPatternsForDependencies } = require("@nrwl/next/tailwind");
const { nextui } = require("@nextui-org/react");

/** @type {import("tailwindcss").Config} */
module.exports = {
   content: [
      join(
         __dirname,
         "{src,pages,components,app}/**/*!(*.stories|*.spec).{ts,tsx,html}"
      ),
      join(
         __dirname,
         "..",
         "..",
         "node_modules/@nextui-org/theme/dist/**/*.{js,ts,jsx,tsx}"
      ),
      ...createGlobPatternsForDependencies(__dirname),
   ],
   theme: {
      extend: {},
   },
   darkMode: "class",
   plugins: [nextui()],
};
