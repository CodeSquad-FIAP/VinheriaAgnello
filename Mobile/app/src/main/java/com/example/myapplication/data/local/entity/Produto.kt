package com.example.myapplication.data.local.entity

import androidx.room.ColumnInfo
import androidx.room.Entity
import androidx.room.PrimaryKey

/**
 * Entidade que representa um Produto (Vinho) no banco de dados local.
 * Mapeada para manter consistência com os contratos do servidor C#.
 */
@Entity(tableName = "produtos")
data class Produto(
    @PrimaryKey(autoGenerate = true)
    val id: Int = 0,

    @ColumnInfo(name = "nome_vinho")
    val nome: String,

    @ColumnInfo(name = "tipo")
    val tipo: String,

    @ColumnInfo(name = "safra")
    val safra: String, // String para suportar "2022" ou "NV" (Non-Vintage)

    @ColumnInfo(name = "quantidade")
    val quantidade: Int,

    @ColumnInfo(name = "preco")
    val preco: Double // Usando Double para simplicidade, BigDecimal exigiria TypeConverter
)
