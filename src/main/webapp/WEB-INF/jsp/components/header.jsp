<%@ page contentType="text/html; charset=UTF-8" pageEncoding="UTF-8" %>
<!DOCTYPE html>
<html lang="pt-BR" class="scroll-smooth">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no">
    <title>Vinheria Agnello | App</title>
    <script src="https://cdn.tailwindcss.com"></script>
    <script src="https://unpkg.com/lucide@latest"></script>
    <link rel="preconnect" href="https://fonts.googleapis.com">
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600&family=Playfair+Display:ital,wght@0,400;0,600;0,700;1,400&display=swap" rel="stylesheet">
    <script>
        tailwind.config = {
            theme: {
                extend: {
                    colors: {
                        wine: {
                            50: '#fbf5f6', 100: '#f7ebed', 200: '#edd2d7', 300: '#e1b0b8',
                            400: '#d08492', 500: '#bc5b6e', 600: '#a34255', 700: '#893344',
                            800: '#732d3b', 900: '#602a34', 950: '#34131a',
                        },
                        sand: {
                            50: '#fdfdfc', 100: '#fbfaf8', 200: '#f5f3ef', 300: '#ebe6dd',
                            400: '#dfd6c7', 500: '#d0c1ab',
                        }
                    },
                    fontFamily: {
                        sans: ['Inter', 'sans-serif'],
                        serif: ['Playfair Display', 'serif'],
                    }
                }
            }
        }
    </script>
    <style>
        .no-scrollbar::-webkit-scrollbar { display: none; }
        .no-scrollbar { -ms-overflow-style: none; scrollbar-width: none; }
        body { overscroll-behavior-y: none; }
    </style>
</head>
<body class="bg-sand-50 text-stone-800 font-sans antialiased pb-20 md:pb-0 flex flex-col min-h-screen">
    <header class="fixed top-0 w-full bg-sand-50/95 backdrop-blur-md z-40 border-b border-sand-200 transition-all">
        <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
            <div class="flex md:justify-between items-center h-16 md:h-20 justify-center">
                <div class="flex-shrink-0 flex items-center gap-2 cursor-pointer" onclick="app.navigate('home')">
                    <i data-lucide="wine" class="text-wine-800 h-6 w-6 md:h-8 md:w-8"></i>
                    <span class="font-serif text-xl md:text-2xl font-semibold tracking-tight text-wine-950">Agnello</span>
                </div>

                <nav class="hidden md:flex space-x-8">
                    <button onclick="app.navigate('home')" class="nav-link text-wine-800 font-medium transition-colors" data-target="home">Início</button>
                    <button onclick="app.navigate('adega')" class="nav-link text-stone-500 hover:text-wine-800 font-medium transition-colors" data-target="adega">Adega</button>
                    <button onclick="app.navigate('sobre')" class="nav-link text-stone-500 hover:text-wine-800 font-medium transition-colors" data-target="sobre">Nossa História</button>
                    <button onclick="app.mockSommelier()" class="text-stone-500 hover:text-wine-800 font-medium transition-colors">Sommelier</button>
                </nav>

                <div class="md:flex items-center gap-4 hidden">
                    <button aria-label="Buscar" onclick="app.navigate('adega', true)" class="text-stone-600 hover:text-wine-800 transition-colors">
                        <i data-lucide="search" class="w-5 h-5 md:w-6 md:h-6"></i>
                    </button>
                    <button aria-label="Carrinho" onclick="app.toggleCart()" class="relative text-stone-600 hover:text-wine-800 transition-colors">
                        <i data-lucide="shopping-bag" class="w-5 h-5 md:w-6 md:h-6"></i>
                        <span id="cart-badge" class="absolute -top-1.5 -right-1.5 bg-wine-800 text-white text-[10px] font-bold w-4 h-4 rounded-full flex items-center justify-center hidden">0</span>
                    </button>
                </div>
            </div>
        </div>
    </header>

    <nav class="md:hidden fixed bottom-0 w-full bg-white border-t border-sand-200 z-40 pb-safe">
        <div class="flex justify-around items-center h-16">
            <button onclick="app.navigate('home')" class="mobile-nav-link flex flex-col items-center gap-1 text-wine-800 w-full" data-target="home">
                <i data-lucide="home" class="w-5 h-5"></i>
                <span class="text-[10px] font-medium">Início</span>
            </button>
            <button onclick="app.navigate('adega')" class="mobile-nav-link flex flex-col items-center gap-1 text-stone-400 hover:text-wine-800 w-full" data-target="adega">
                <i data-lucide="layout-grid" class="w-5 h-5"></i>
                <span class="text-[10px] font-medium">Adega</span>
            </button>
            <button onclick="app.navigate('sobre')" class="mobile-nav-link flex flex-col items-center gap-1 text-stone-400 hover:text-wine-800 w-full" data-target="sobre">
                <i data-lucide="book-open" class="w-5 h-5"></i>
                <span class="text-[10px] font-medium">História</span>
            </button>
            <button onclick="app.toggleCart()" class="flex flex-col items-center gap-1 text-stone-400 hover:text-wine-800 w-full relative">
                <i data-lucide="shopping-bag" class="w-5 h-5"></i>
                <span class="text-[10px] font-medium">Sacola</span>
                <span id="mobile-cart-badge" class="absolute top-0 right-[25%] bg-wine-800 text-white text-[9px] font-bold w-3.5 h-3.5 rounded-full flex items-center justify-center hidden">0</span>
            </button>
        </div>
    </nav>