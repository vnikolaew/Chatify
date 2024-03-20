const { withNxMetro } = require("@nx/expo");
const { getDefaultConfig } = require("@expo/metro-config");
const { mergeConfig } = require("metro-config");
const path = require("node:path");

const defaultConfig = getDefaultConfig(__dirname);
const { assetExts, sourceExts } = defaultConfig.resolver;

const monorepoRoot = path.resolve(__dirname, "..", "..");
const monorepoPackages = {
   "@web/utils": path.resolve(monorepoRoot, "libs/utils"),
   "@web/api": path.resolve(monorepoRoot, "libs/api"),
   "@web/hooks": path.resolve(monorepoRoot, "libs/hooks"),
};
/**
 * Metro configuration
 * https://facebook.github.io/metro/docs/configuration
 *
 * @type {import('metro-config').MetroConfig}
 */
const customConfig = {
   transformer: {
      babelTransformerPath: require.resolve("react-native-svg-transformer"),
   },
   resolver: {
      assetExts: assetExts.filter((ext) => ext !== "svg"),
      sourceExts: [...sourceExts, "cjs", "mjs", "svg"],
      unstable_enableSymlinks: true,
      resolverMainFields: [`native`, `module`, `browser`, `main`],
      nodeModulesPaths: [
         path.resolve(__dirname, "node_modules"),
         path.resolve(monorepoRoot, "node_modules"),
      ],
      unstable_conditionNames: ["browser", "require", "react-native"],
      extraNodeModules: monorepoPackages,
   },
};

module.exports = withNxMetro(mergeConfig(defaultConfig, customConfig), {
   // Change this to true to see debugging info.
   // Useful if you have issues resolving modules
   debug: false,
   // all the file extensions used for imports other than 'ts', 'tsx', 'js', 'jsx', 'json'
   extensions: [],
   // Specify folders to watch, in addition to Nx defaults (workspace libraries and node_modules)
   watchFolders: [__dirname, ...Object.values(monorepoPackages)],
}).then((config) => {
   return config;
});
