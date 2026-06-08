module.exports = {
  content: [
    "./Views/**/*.cshtml",
    "./wwwroot/js/**/*.js"
  ],
  theme: {
    extend: {
      colors: {
        emit: {
          navy: "#17203A",
          deep: "#12192B",
          blue: "#73A5D4",
          mid: "#5C8ABF"
        }
      }
    }
  },
  plugins: []
};
