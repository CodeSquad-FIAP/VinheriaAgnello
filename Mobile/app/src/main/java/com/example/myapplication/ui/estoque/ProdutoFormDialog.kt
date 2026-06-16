package com.example.myapplication.ui.estoque

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.material3.AlertDialog
import androidx.compose.material3.Button
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.unit.dp
import com.example.myapplication.data.local.entity.Produto

@Composable
fun ProdutoFormDialog(
    produtoInicial: Produto?,
    onConfirmar: (Produto) -> Unit,
    onDismiss: () -> Unit
) {
    val isEdicao = produtoInicial != null

    var nome by remember { mutableStateOf(produtoInicial?.nome ?: "") }
    var tipo by remember { mutableStateOf(produtoInicial?.tipo ?: "") }
    var safra by remember { mutableStateOf(produtoInicial?.safra ?: "") }
    var quantidade by remember { mutableStateOf(produtoInicial?.quantidade?.toString() ?: "") }
    var preco by remember { mutableStateOf(produtoInicial?.preco?.toString() ?: "") }
    var nomeError by remember { mutableStateOf(false) }

    AlertDialog(
        onDismissRequest = onDismiss,
        title = { Text(if (isEdicao) "Editar vinho" else "Novo vinho") },
        text = {
            Column(verticalArrangement = Arrangement.spacedBy(12.dp)) {
                OutlinedTextField(
                    value = nome,
                    onValueChange = { nome = it; nomeError = false },
                    label = { Text("Nome *") },
                    isError = nomeError,
                    supportingText = if (nomeError) {{ Text("Campo obrigatório") }} else null,
                    singleLine = true,
                    modifier = Modifier.fillMaxWidth()
                )
                OutlinedTextField(
                    value = tipo,
                    onValueChange = { tipo = it },
                    label = { Text("Tipo") },
                    placeholder = { Text("Tinto, Branco, Rosé…") },
                    singleLine = true,
                    modifier = Modifier.fillMaxWidth()
                )
                OutlinedTextField(
                    value = safra,
                    onValueChange = { safra = it },
                    label = { Text("Safra") },
                    placeholder = { Text("2022 ou NV") },
                    singleLine = true,
                    modifier = Modifier.fillMaxWidth()
                )
                Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                    OutlinedTextField(
                        value = quantidade,
                        onValueChange = { if (it.all(Char::isDigit)) quantidade = it },
                        label = { Text("Qtd.") },
                        keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Number),
                        singleLine = true,
                        modifier = Modifier.weight(1f)
                    )
                    OutlinedTextField(
                        value = preco,
                        onValueChange = { if (it.isEmpty() || it.matches(Regex("^\\d*[,.]?\\d*$"))) preco = it },
                        label = { Text("Preço (R$)") },
                        keyboardOptions = KeyboardOptions(keyboardType = KeyboardType.Decimal),
                        singleLine = true,
                        modifier = Modifier.weight(1f)
                    )
                }
            }
        },
        confirmButton = {
            Button(onClick = {
                if (nome.isBlank()) { nomeError = true; return@Button }
                onConfirmar(
                    Produto(
                        id = produtoInicial?.id ?: 0,
                        nome = nome.trim(),
                        tipo = tipo.trim(),
                        safra = safra.trim(),
                        quantidade = quantidade.toIntOrNull() ?: 0,
                        preco = preco.replace(",", ".").toDoubleOrNull() ?: 0.0
                    )
                )
            }) {
                Text(if (isEdicao) "Salvar" else "Adicionar")
            }
        },
        dismissButton = {
            TextButton(onClick = onDismiss) { Text("Cancelar") }
        }
    )
}
