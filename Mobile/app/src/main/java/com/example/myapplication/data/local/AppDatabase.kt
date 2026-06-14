package com.example.myapplication.data.local

import androidx.room.Database
import androidx.room.RoomDatabase
import com.example.myapplication.data.local.dao.ProdutoDao
import com.example.myapplication.data.local.entity.Produto

/**
 * Banco de dados principal da aplicação utilizando Room.
 * Registra a entidade Produto e exporta o schema.
 */
@Database(entities = [Produto::class], version = 1, exportSchema = false)
abstract class AppDatabase : RoomDatabase() {
    abstract fun produtoDao(): ProdutoDao
}
