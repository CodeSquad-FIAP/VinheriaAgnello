package com.example.myapplication.ui.estoque

import android.content.Context
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Add
import androidx.compose.material.icons.filled.Clear
import androidx.compose.material.icons.filled.Search
import androidx.compose.material3.AlertDialog
import androidx.compose.material3.CardDefaults
import androidx.compose.material3.ElevatedCard
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.FloatingActionButton
import androidx.compose.material3.HorizontalDivider
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Scaffold
import androidx.compose.material3.SwipeToDismissBox
import androidx.compose.material3.SwipeToDismissBoxValue
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.material3.TextField
import androidx.compose.material3.TextFieldDefaults
import androidx.compose.material3.VerticalDivider
import androidx.compose.material3.rememberSwipeToDismissBoxState
import androidx.compose.runtime.Composable
import androidx.compose.runtime.derivedStateOf
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.rememberUpdatedState
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import com.example.myapplication.data.local.entity.Produto
import java.text.Normalizer
import java.text.NumberFormat
import java.util.Locale

private val brCurrency = NumberFormat.getCurrencyInstance(Locale("pt", "BR"))

private fun String.semAcentos(): String =
    Normalizer.normalize(this, Normalizer.Form.NFD)
        .replace(Regex("\\p{Mn}"), "")

@Composable
fun EstoqueScreen(
    produtos: List<Produto>,
    onAdicionar: (Produto) -> Unit,
    onAtualizar: (Produto) -> Unit,
    onDeletar: (Produto) -> Unit,
    modifier: Modifier = Modifier
) {
    var showDialog by remember { mutableStateOf(false) }
    var produtoEmEdicao by remember { mutableStateOf<Produto?>(null) }
    var searchQuery by remember { mutableStateOf("") }
    val produtosFiltrados by remember(produtos, searchQuery) {
        derivedStateOf {
            if (searchQuery.isBlank()) produtos
            else {
                val query = searchQuery.semAcentos().lowercase()
                produtos.filter {
                    it.nome.semAcentos().contains(query, ignoreCase = true) ||
                    it.tipo.semAcentos().contains(query, ignoreCase = true) ||
                    it.safra.semAcentos().contains(query, ignoreCase = true)
                }
            }
        }
    }

    val context = LocalContext.current
    val prefs = remember { context.getSharedPreferences("app_prefs", Context.MODE_PRIVATE) }
    var mostrarDica by remember { mutableStateOf(!prefs.getBoolean("swipe_used", false)) }
    val onSwipeUsado = remember(prefs) {
        {
            mostrarDica = false
            prefs.edit().putBoolean("swipe_used", true).apply()
        }
    }

    Scaffold(
        modifier = modifier,
        floatingActionButton = {
            FloatingActionButton(onClick = {
                produtoEmEdicao = null
                showDialog = true
            }) {
                Icon(Icons.Default.Add, contentDescription = "Adicionar vinho")
            }
        }
    ) { innerPadding ->
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(innerPadding)
        ) {
            ResumoCard(
                produtos = produtos,
                modifier = Modifier.padding(horizontal = 16.dp, vertical = 12.dp)
            )

            TextField(
                value = searchQuery,
                onValueChange = { searchQuery = it },
                placeholder = { Text("Buscar por nome, tipo ou safra…") },
                leadingIcon = { Icon(Icons.Default.Search, contentDescription = null) },
                trailingIcon = {
                    if (searchQuery.isNotBlank()) {
                        IconButton(onClick = { searchQuery = "" }) {
                            Icon(Icons.Default.Clear, contentDescription = "Limpar busca")
                        }
                    }
                },
                singleLine = true,
                shape = RoundedCornerShape(percent = 50),
                colors = TextFieldDefaults.colors(
                    focusedIndicatorColor = Color.Transparent,
                    unfocusedIndicatorColor = Color.Transparent,
                    disabledIndicatorColor = Color.Transparent,
                ),
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(horizontal = 16.dp)
                    .padding(bottom = 8.dp)
            )

            if (produtos.isEmpty()) {
                Box(
                    modifier = Modifier.fillMaxSize(),
                    contentAlignment = Alignment.Center
                ) {
                    Column(horizontalAlignment = Alignment.CenterHorizontally) {
                        Text(
                            text = "Nenhum vinho no estoque",
                            style = MaterialTheme.typography.bodyLarge
                        )
                        Spacer(Modifier.height(8.dp))
                        Text(
                            text = "Toque em + para adicionar",
                            style = MaterialTheme.typography.bodyMedium,
                            color = MaterialTheme.colorScheme.onSurfaceVariant
                        )
                    }
                }
            } else if (produtosFiltrados.isEmpty()) {
                Box(
                    modifier = Modifier.fillMaxSize(),
                    contentAlignment = Alignment.Center
                ) {
                    Text(
                        text = "Nenhum resultado para \"$searchQuery\"",
                        style = MaterialTheme.typography.bodyMedium,
                        color = MaterialTheme.colorScheme.onSurfaceVariant
                    )
                }
            } else {
                LazyColumn(
                    modifier = Modifier
                        .fillMaxSize()
                        .padding(horizontal = 16.dp),
                    verticalArrangement = Arrangement.spacedBy(8.dp),
                    contentPadding = PaddingValues(bottom = 88.dp)
                ) {
                    items(produtosFiltrados, key = { it.id }) { produto ->
                        ProdutoCard(
                            produto = produto,
                            mostrarDica = mostrarDica,
                            onSwipeUsado = onSwipeUsado,
                            onEditar = {
                                produtoEmEdicao = it
                                showDialog = true
                            },
                            onDeletar = onDeletar
                        )
                    }
                }
            }
        }
    }

    if (showDialog) {
        ProdutoFormDialog(
            produtoInicial = produtoEmEdicao,
            onConfirmar = { produto ->
                if (produtoEmEdicao == null) onAdicionar(produto) else onAtualizar(produto)
                showDialog = false
            },
            onDismiss = { showDialog = false }
        )
    }
}

@Composable
private fun ResumoCard(produtos: List<Produto>, modifier: Modifier = Modifier) {
    val totalGarrafas = produtos.sumOf { it.quantidade }
    val valorTotal = produtos.sumOf { it.quantidade * it.preco }

    ElevatedCard(
        modifier = modifier.fillMaxWidth(),
        colors = CardDefaults.elevatedCardColors(
            containerColor = MaterialTheme.colorScheme.primaryContainer
        )
    ) {
        Row(
            modifier = Modifier
                .fillMaxWidth()
                .padding(vertical = 16.dp),
            horizontalArrangement = Arrangement.SpaceEvenly,
            verticalAlignment = Alignment.CenterVertically
        ) {
            ResumoItem(label = "Vinhos", value = "${produtos.size}")
            VerticalDivider(
                modifier = Modifier.height(32.dp),
                color = Color.White.copy(alpha = 0.35f)
            )
            ResumoItem(label = "Garrafas", value = "$totalGarrafas")
            VerticalDivider(
                modifier = Modifier.height(32.dp),
                color = Color.White.copy(alpha = 0.35f)
            )
            ResumoItem(label = "Valor total", value = brCurrency.format(valorTotal))
        }
    }
}

@Composable
private fun ResumoItem(label: String, value: String) {
    Column(horizontalAlignment = Alignment.CenterHorizontally) {
        Text(
            text = value,
            style = MaterialTheme.typography.titleLarge,
            fontWeight = FontWeight.Bold,
            color = MaterialTheme.colorScheme.onPrimaryContainer
        )
        Text(
            text = label,
            style = MaterialTheme.typography.labelSmall,
            color = MaterialTheme.colorScheme.onPrimaryContainer.copy(alpha = 0.7f)
        )
    }
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
private fun ProdutoCard(
    produto: Produto,
    mostrarDica: Boolean,
    onSwipeUsado: () -> Unit,
    onEditar: (Produto) -> Unit,
    onDeletar: (Produto) -> Unit
) {
    var confirmDelete by remember { mutableStateOf(false) }

    val currentProduto by rememberUpdatedState(produto)
    val currentOnEditar by rememberUpdatedState(onEditar)
    val currentOnSwipeUsado by rememberUpdatedState(onSwipeUsado)

    val dismissState = rememberSwipeToDismissBoxState(
        confirmValueChange = { value ->
            when (value) {
                SwipeToDismissBoxValue.EndToStart -> { currentOnSwipeUsado(); currentOnEditar(currentProduto) }
                SwipeToDismissBoxValue.StartToEnd -> { currentOnSwipeUsado(); confirmDelete = true }
                else -> {}
            }
            false
        }
    )

    SwipeToDismissBox(
        state = dismissState,
        enableDismissFromStartToEnd = true,
        enableDismissFromEndToStart = true,
        backgroundContent = {
            val offset = runCatching { dismissState.requireOffset() }.getOrDefault(0f)
            val isExcluir = offset > 0f
            val isEditar  = offset < 0f

            val bgColor = when {
                isExcluir -> MaterialTheme.colorScheme.errorContainer
                isEditar  -> MaterialTheme.colorScheme.secondaryContainer
                else      -> Color.Transparent
            }
            val textColor = when {
                isExcluir -> MaterialTheme.colorScheme.onErrorContainer
                isEditar  -> MaterialTheme.colorScheme.onSecondaryContainer
                else      -> Color.Transparent
            }

            Box(
                modifier = Modifier
                    .fillMaxSize()
                    .clip(RoundedCornerShape(12.dp))
                    .background(bgColor)
                    .padding(horizontal = 20.dp),
                contentAlignment = if (isExcluir) Alignment.CenterStart else Alignment.CenterEnd
            ) {
                Text(
                    text = if (isExcluir) "Excluir" else "Editar",
                    style = MaterialTheme.typography.labelLarge,
                    fontWeight = FontWeight.Bold,
                    color = textColor
                )
            }
        }
    ) {
        ElevatedCard(modifier = Modifier.fillMaxWidth()) {
            Column(modifier = Modifier.padding(16.dp)) {
                Text(
                    text = produto.nome,
                    style = MaterialTheme.typography.titleMedium,
                    fontWeight = FontWeight.Bold
                )
                Spacer(Modifier.height(2.dp))
                Text(
                    text = buildString {
                        if (produto.tipo.isNotBlank()) append(produto.tipo)
                        if (produto.tipo.isNotBlank() && produto.safra.isNotBlank()) append(" • ")
                        if (produto.safra.isNotBlank()) append("Safra ${produto.safra}")
                    },
                    style = MaterialTheme.typography.bodySmall,
                    color = MaterialTheme.colorScheme.onSurfaceVariant
                )

                Spacer(Modifier.height(8.dp))
                HorizontalDivider()
                Spacer(Modifier.height(8.dp))

                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.SpaceBetween
                ) {
                    InfoLabel(label = "Quantidade", value = "${produto.quantidade} garrafas")
                    InfoLabel(label = "Preço un.", value = brCurrency.format(produto.preco))
                }

                if (mostrarDica) {
                    Spacer(Modifier.height(8.dp))
                    Row(
                        modifier = Modifier.fillMaxWidth(),
                        horizontalArrangement = Arrangement.SpaceBetween
                    ) {
                        Text(
                            text = "‹ editar",
                            style = MaterialTheme.typography.labelSmall,
                            color = MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.4f)
                        )
                        Text(
                            text = "excluir ›",
                            style = MaterialTheme.typography.labelSmall,
                            color = MaterialTheme.colorScheme.onSurfaceVariant.copy(alpha = 0.4f)
                        )
                    }
                }
            }
        }
    }

    if (confirmDelete) {
        AlertDialog(
            onDismissRequest = { confirmDelete = false },
            title = { Text("Excluir vinho") },
            text = { Text("Excluir \"${produto.nome}\" do estoque? Esta ação não pode ser desfeita.") },
            confirmButton = {
                TextButton(onClick = { onDeletar(produto); confirmDelete = false }) {
                    Text("Excluir", color = MaterialTheme.colorScheme.error)
                }
            },
            dismissButton = {
                TextButton(onClick = { confirmDelete = false }) { Text("Cancelar") }
            }
        )
    }
}

@Composable
private fun InfoLabel(label: String, value: String) {
    Column {
        Text(
            text = label,
            style = MaterialTheme.typography.labelSmall,
            color = MaterialTheme.colorScheme.onSurfaceVariant
        )
        Text(
            text = value,
            style = MaterialTheme.typography.bodyMedium,
            fontWeight = FontWeight.SemiBold
        )
    }
}
