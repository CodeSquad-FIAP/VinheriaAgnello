package com.example.myapplication.ui.estoque

import androidx.lifecycle.ViewModel
import androidx.lifecycle.ViewModelProvider
import androidx.lifecycle.viewModelScope
import com.example.myapplication.data.local.entity.Produto
import com.example.myapplication.data.repository.ProdutoRepository
import kotlinx.coroutines.flow.SharingStarted
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.stateIn
import kotlinx.coroutines.launch

class EstoqueViewModel(private val repository: ProdutoRepository) : ViewModel() {

    val produtos: StateFlow<List<Produto>> = repository.getAllProdutos()
        .stateIn(viewModelScope, SharingStarted.WhileSubscribed(5_000), emptyList())

    fun inserir(produto: Produto) = viewModelScope.launch { repository.inserir(produto) }
    fun atualizar(produto: Produto) = viewModelScope.launch { repository.atualizar(produto) }
    fun deletar(produto: Produto) = viewModelScope.launch { repository.deletar(produto) }
}

class EstoqueViewModelFactory(
    private val repository: ProdutoRepository
) : ViewModelProvider.Factory {
    @Suppress("UNCHECKED_CAST")
    override fun <T : ViewModel> create(modelClass: Class<T>): T =
        EstoqueViewModel(repository) as T
}
