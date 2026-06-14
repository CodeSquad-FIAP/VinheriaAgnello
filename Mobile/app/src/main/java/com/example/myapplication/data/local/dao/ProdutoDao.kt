package com.example.myapplication.data.local.dao

import androidx.room.*
import com.example.myapplication.data.local.entity.Produto
import kotlinx.coroutines.flow.Flow

/**
 * Data Access Object (DAO) para operações de CRUD na tabela de produtos.
 * Segue o Princípio da Responsabilidade Única (SRP).
 */
@Dao
interface ProdutoDao {

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    suspend fun insert(produto: Produto): Long

    @Update
    suspend fun update(produto: Produto)

    @Delete
    suspend fun delete(produto: Produto)

    @Query("SELECT * FROM produtos")
    fun getAllProdutos(): Flow<List<Produto>>

    @Query("SELECT * FROM produtos WHERE id = :id")
    suspend fun getProdutoById(id: Int): Produto?
}
