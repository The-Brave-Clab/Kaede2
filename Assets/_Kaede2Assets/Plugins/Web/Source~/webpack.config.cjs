const path = require('path');

module.exports = {
  entry: './index.js', // Main file
  output: {
    filename: 'bundle.js', // The output bundle
    path: path.resolve(__dirname, 'dist'), // Output directory
    library: 'AWS', // Name of the library
    libraryTarget: 'window', // Export to window
  },
  mode: 'production', // Production mode
  devtool: false, // No source maps
};