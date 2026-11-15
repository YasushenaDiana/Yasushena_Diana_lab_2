<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="html" encoding="utf-8" indent="yes" />

  <xsl:template match="/">
    <html lang="uk">
      <head>
        <title>Кадри науковців (Звання)</title>
        <style>
          body { font-family: Arial, sans-serif; background: #EEF1F5; color: #1E2833; margin: 32px; }
          h1 { color: #1E2833; font-size: 24px; }
          p.count { color: #66717E; }
          table { border-collapse: collapse; width: 100%; background: #FFFFFF; }
          th { background: #2F5D8A; color: #FFFFFF; text-align: left; padding: 10px; }
          td { border-bottom: 1px solid #D5DBE3; padding: 9px 10px; vertical-align: top; }
          tr:nth-child(even) td { background: #F9FAFC; }
          .empty { color: #66717E; }
          ul { margin: 0; padding-left: 18px; }
        </style>
      </head>
      <body>
        <h1>Кадри науковців (Звання)</h1>
        <p class="count">
          Кількість науковців: <xsl:value-of select="count(/Scientists/Scientist)" />
        </p>
        <table>
          <tr>
            <th>№</th>
            <th>П.І.П.</th>
            <th>Факультет</th>
            <th>Кафедра</th>
            <th>Науковий ступінь</th>
            <th>Вчене звання (дата присвоєння)</th>
          </tr>
          <xsl:for-each select="/Scientists/Scientist">
            <xsl:sort select="normalize-space(@FullName)" lang="uk" />
            <tr>
              <td><xsl:value-of select="position()" /></td>
              <td><xsl:value-of select="normalize-space(@FullName)" /></td>
              <td><xsl:call-template name="value-or-dash"><xsl:with-param name="value" select="@Faculty" /></xsl:call-template></td>
              <td><xsl:call-template name="value-or-dash"><xsl:with-param name="value" select="@Department" /></xsl:call-template></td>
              <td><xsl:call-template name="value-or-dash"><xsl:with-param name="value" select="@Degree" /></xsl:call-template></td>
              <td>
                <xsl:choose>
                  <xsl:when test="Titles/Title">
                    <ul>
                      <xsl:for-each select="Titles/Title">
                        <xsl:sort select="@AssignedDate" />
                        <li>
                          <xsl:value-of select="normalize-space(@Name)" />
                          <xsl:text> (</xsl:text>
                          <xsl:choose>
                            <xsl:when test="@AssignedDate">
                              <xsl:call-template name="format-date">
                                <xsl:with-param name="date" select="@AssignedDate" />
                              </xsl:call-template>
                            </xsl:when>
                            <xsl:otherwise>дата не вказана</xsl:otherwise>
                          </xsl:choose>
                          <xsl:text>)</xsl:text>
                        </li>
                      </xsl:for-each>
                    </ul>
                  </xsl:when>
                  <xsl:otherwise><span class="empty">-</span></xsl:otherwise>
                </xsl:choose>
              </td>
            </tr>
          </xsl:for-each>
        </table>
      </body>
    </html>
  </xsl:template>

  <xsl:template name="value-or-dash">
    <xsl:param name="value" />
    <xsl:choose>
      <xsl:when test="normalize-space($value) != ''"><xsl:value-of select="normalize-space($value)" /></xsl:when>
      <xsl:otherwise><span class="empty">-</span></xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="format-date">
    <xsl:param name="date" />
    <xsl:variable name="d" select="normalize-space($date)" />
    <xsl:choose>
      <xsl:when test="string-length($d) = 10 and substring($d, 5, 1) = '-' and substring($d, 8, 1) = '-'">
        <xsl:value-of select="concat(substring($d, 9, 2), '.', substring($d, 6, 2), '.', substring($d, 1, 4))" />
      </xsl:when>
      <xsl:otherwise><xsl:value-of select="$d" /></xsl:otherwise>
    </xsl:choose>
  </xsl:template>
</xsl:stylesheet>
