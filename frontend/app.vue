<template>
  <div id="q-app" class="locallink-app">
    <NuxtPage />
  </div>
</template>

<style>
@import url('https://fonts.googleapis.com/css2?family=Outfit:wght@300;400;500;600;700;800&family=Plus+Jakarta+Sans:wght@400;500;600;700&display=swap');

:root {
  --ll-primary: #0f172a;
  --ll-secondary: #0284c7;
  --ll-accent: #10b981;
  --ll-bg: #0b0f19;
  --ll-surface: #111827;
  --ll-border: #1f2937;
  --ll-text: #f8fafc;
  --ll-text-muted: #94a3b8;
}

* {
  box-sizing: border-box;
  margin: 0;
  padding: 0;
}

body {
  font-family: 'Plus Jakarta Sans', sans-serif;
  background-color: var(--ll-bg);
  color: var(--ll-text);
  min-height: 100vh;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
}

h1, h2, h3, h4, h5, h6 {
  font-family: 'Outfit', sans-serif;
  font-weight: 700;
}
</style>
