package com.example.myapplication.ui.theme

import androidx.compose.foundation.isSystemInDarkTheme
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.darkColorScheme
import androidx.compose.material3.lightColorScheme
import androidx.compose.runtime.Composable
import androidx.compose.ui.graphics.Color

private val LightColorScheme = lightColorScheme(
    primary = Wine800,
    onPrimary = Color.White,
    primaryContainer = Wine100,
    onPrimaryContainer = Wine950,
    secondary = Stone500,
    onSecondary = Color.White,
    secondaryContainer = Sand300,
    onSecondaryContainer = Stone900,
    background = Sand50,
    onBackground = Stone900,
    surface = Color.White,
    onSurface = Stone900,
    surfaceVariant = Sand100,
    onSurfaceVariant = Stone500,
    outline = Stone400,
    error = Color(0xFFB3261E),
    onError = Color.White,
)

private val DarkColorScheme = darkColorScheme(
    primary = Wine300,
    onPrimary = Wine950,
    primaryContainer = Wine800,
    onPrimaryContainer = Wine100,
    secondary = Wine400,
    onSecondary = Wine950,
    secondaryContainer = Color(0xFF3A2028),
    onSecondaryContainer = Wine100,
    background = DarkBg,
    onBackground = DarkOnSurface,
    surface = DarkSurface,
    onSurface = DarkOnSurface,
    surfaceVariant = DarkSurface2,
    onSurfaceVariant = DarkMuted,
    outline = Color(0xFF6B4A52),
    error = Color(0xFFF2B8B8),
    onError = Color(0xFF601410),
)

@Composable
fun MyApplicationTheme(
    darkTheme: Boolean = isSystemInDarkTheme(),
    content: @Composable () -> Unit
) {
    val colorScheme = if (darkTheme) DarkColorScheme else LightColorScheme

    MaterialTheme(
        colorScheme = colorScheme,
        typography = Typography,
        content = content
    )
}
