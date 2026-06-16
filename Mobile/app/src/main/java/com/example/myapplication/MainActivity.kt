package com.example.myapplication

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.enableEdgeToEdge
import androidx.activity.viewModels
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Inventory
import androidx.compose.material.icons.filled.Person
import androidx.compose.material.icons.filled.WineBar
import androidx.compose.material3.Icon
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.material3.adaptive.navigationsuite.NavigationSuiteScaffold
import androidx.compose.runtime.Composable
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.vector.ImageVector
import androidx.room.Room
import com.example.myapplication.data.local.AppDatabase
import com.example.myapplication.data.repository.ProdutoRepository
import com.example.myapplication.ui.estoque.EstoqueScreen
import com.example.myapplication.ui.estoque.EstoqueViewModel
import com.example.myapplication.ui.estoque.EstoqueViewModelFactory
import com.example.myapplication.ui.theme.MyApplicationTheme

class MainActivity : ComponentActivity() {

    private val database by lazy {
        Room.databaseBuilder(
            applicationContext,
            AppDatabase::class.java,
            "vinheria_agnello.db"
        ).build()
    }

    private val viewModel: EstoqueViewModel by viewModels {
        EstoqueViewModelFactory(ProdutoRepository(database.produtoDao()))
    }

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        enableEdgeToEdge()
        setContent {
            MyApplicationTheme {
                VinheriaApp(viewModel)
            }
        }
    }
}

@Composable
fun VinheriaApp(viewModel: EstoqueViewModel) {
    var currentDestination by remember { mutableStateOf(AppDestinations.ESTOQUE) }
    val produtos by viewModel.produtos.collectAsState()

    NavigationSuiteScaffold(
        navigationSuiteItems = {
            AppDestinations.entries.forEach { dest ->
                item(
                    icon = {
                        Icon(
                            imageVector = dest.icon,
                            contentDescription = dest.label
                        )
                    },
                    label = { Text(dest.label) },
                    selected = dest == currentDestination,
                    onClick = { currentDestination = dest }
                )
            }
        }
    ) {
        when (currentDestination) {
            AppDestinations.ESTOQUE -> EstoqueScreen(
                produtos = produtos,
                onAdicionar = viewModel::inserir,
                onAtualizar = viewModel::atualizar,
                onDeletar = viewModel::deletar,
                modifier = Modifier.fillMaxSize()
            )
            AppDestinations.VINHOS -> PlaceholderScreen("Vinhos em destaque")
            AppDestinations.PERFIL -> PlaceholderScreen("Perfil do usuário")
        }
    }
}

@Composable
private fun PlaceholderScreen(texto: String) {
    Box(modifier = Modifier.fillMaxSize(), contentAlignment = Alignment.Center) {
        Text(texto, style = MaterialTheme.typography.bodyLarge)
    }
}

enum class AppDestinations(val label: String, val icon: ImageVector) {
    ESTOQUE("Estoque", Icons.Default.Inventory),
    VINHOS("Vinhos", Icons.Default.WineBar),
    PERFIL("Perfil", Icons.Default.Person),
}
